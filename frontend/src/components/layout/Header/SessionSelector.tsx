import { useState, useEffect, useRef, useMemo } from 'react';
import {
  Autocomplete,
  TextField,
  Box,
  Typography,
  Chip,
} from '@mui/material';
import { useAuth } from '../../../hooks/useAuth';
import { getAcademicSessions } from '../../../api/academicSession.api';
import { getBatches } from '../../../api/batch.api';
import { getInstitutes } from '../../../api/institute.api';
import type { AcademicSession, Institute } from '../../../types/common.types';

interface DeduplicatedSession {
  sessionYear: string;
  startDate: string;
  endDate: string;
  hasActive: boolean;
}

export function SessionSelector() {
  const { user, switchSession, switchSessionYear, isSwitchingSession } = useAuth();
  const isSuperAdmin = user?.role === 'Admin';

  const [sessions, setSessions] = useState<AcademicSession[]>([]);
  const [institutes, setInstitutes] = useState<Institute[]>([]);
  const [loading, setLoading] = useState(false);
  const autoSwitchAttempted = useRef(false);

  useEffect(() => {
    setLoading(true);
    autoSwitchAttempted.current = false;

    const fetchSessions = async () => {
      try {
        if (isSuperAdmin) {
          const [sessionsRes, institutesRes] = await Promise.all([
            getAcademicSessions({ pageNumber: 1, pageSize: 200 }),
            getInstitutes({ pageNumber: 1, pageSize: 200 }),
          ]);
          setSessions(sessionsRes.items);
          setInstitutes(institutesRes.items);
        } else if (user?.role === 'TradeHead' && user?.tradeId) {
          const batchesRes = await getBatches({ tradeId: user.tradeId, pageNumber: 1, pageSize: 100 });
          const sessionIds = [...new Set(
            batchesRes.items
              .map(b => b.startAcademicSessionId)
              .filter((id): id is string => !!id)
          )];

          if (sessionIds.length === 0) {
            setSessions([]);
            return;
          }

          const allSessions = await getAcademicSessions({ pageNumber: 1, pageSize: 100 });
          setSessions(allSessions.items.filter(s => sessionIds.includes(s.id)));
        } else {
          const res = await getAcademicSessions({ pageNumber: 1, pageSize: 100 });
          setSessions(res.items);
        }
      } catch {
        // Silently ignore
      } finally {
        setLoading(false);
      }
    };

    fetchSessions();
  }, [isSuperAdmin, user?.role, user?.tradeId]);

  const deduplicatedSessions = useMemo(() => {
    if (!isSuperAdmin) return null;
    const map = new Map<string, DeduplicatedSession>();
    for (const s of sessions) {
      const existing = map.get(s.sessionYear);
      if (existing) {
        existing.hasActive = existing.hasActive || s.isActive;
      } else {
        map.set(s.sessionYear, {
          sessionYear: s.sessionYear,
          startDate: s.startDate,
          endDate: s.endDate,
          hasActive: s.isActive,
        });
      }
    }
    return Array.from(map.values()).sort((a, b) => b.sessionYear.localeCompare(a.sessionYear));
  }, [sessions, isSuperAdmin]);

  useEffect(() => {
    if (autoSwitchAttempted.current || loading || sessions.length === 0) return;

    if (isSuperAdmin) {
      if (user?.sessionYear) return;
      autoSwitchAttempted.current = true;
      const activeSession = sessions.find(s => s.isActive);
      if (activeSession) {
        switchSessionYear(activeSession.sessionYear);
      } else if (sessions.length > 0) {
        switchSessionYear(sessions[0].sessionYear);
      }
    } else {
      if (user?.academicSessionId) return;
      autoSwitchAttempted.current = true;
      const activeSession = sessions.find(s => s.isActive) || sessions[0];
      if (activeSession) {
        switchSession(activeSession.id);
      }
    }
  }, [sessions, loading, user?.academicSessionId, user?.sessionYear, isSuperAdmin, switchSession, switchSessionYear]);

  if (isSuperAdmin) {
    const selectedYear = deduplicatedSessions?.find(d => d.sessionYear === user?.sessionYear) || null;
    const selectedInstitute = institutes.find(i => i.id === user?.instituteFilterId) || null;

    const handleYearChange = (_: unknown, newValue: DeduplicatedSession | null) => {
      if (!newValue) return;
      switchSessionYear(newValue.sessionYear, user?.instituteFilterId);
    };

    const handleInstituteChange = (_: unknown, newValue: Institute | null) => {
      switchSessionYear(user?.sessionYear || '', newValue?.id);
    };

    return (
      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
        <Autocomplete
          size="small"
          options={deduplicatedSessions || []}
          getOptionLabel={(option) => option.sessionYear}
          value={selectedYear}
          onChange={handleYearChange}
          loading={loading}
          isOptionEqualToValue={(option, value) => option.sessionYear === value.sessionYear}
          renderOption={(props, option) => (
            <li {...props} key={option.sessionYear}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                <Typography variant="body2" sx={{ flex: 1 }}>
                  {option.sessionYear}
                </Typography>
                {option.hasActive && (
                  <Chip label="Active" color="success" size="small" sx={{ height: 20, fontSize: 10 }} />
                )}
              </Box>
            </li>
          )}
          renderInput={(params) => (
            <TextField
              {...params}
              placeholder="Session"
              variant="outlined"
              sx={{
                minWidth: 140,
                '& .MuiOutlinedInput-root': {
                  height: 32,
                  fontSize: 12,
                  '& fieldset': { borderColor: 'rgba(255,255,255,0.15)' },
                  '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.3)' },
                },
              }}
            />
          )}
          sx={{
            width: 140,
            '& .MuiAutocomplete-inputRoot': { pr: '28px !important' },
          }}
        />
        <Autocomplete
          size="small"
          options={[{ id: '', name: 'All Institutes' } as Institute, ...institutes]}
          getOptionLabel={(option) => option.name}
          value={selectedInstitute}
          onChange={handleInstituteChange}
          isOptionEqualToValue={(option, value) => option.id === value.id}
          renderInput={(params) => (
            <TextField
              {...params}
              placeholder="Institute"
              variant="outlined"
              sx={{
                minWidth: 140,
                '& .MuiOutlinedInput-root': {
                  height: 32,
                  fontSize: 12,
                  '& fieldset': { borderColor: 'rgba(255,255,255,0.15)' },
                  '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.3)' },
                },
              }}
            />
          )}
          sx={{
            width: 160,
            '& .MuiAutocomplete-inputRoot': { pr: '28px !important' },
          }}
        />
      </Box>
    );
  }

  const activeSession = sessions.find((s) => s.id === user?.academicSessionId);

  const handleChange = async (_: unknown, newValue: AcademicSession | null) => {
    if (!newValue || newValue.id === user?.academicSessionId) return;
    await switchSession(newValue.id);
  };

  return (
    <Autocomplete
      size="small"
      options={sessions}
      getOptionLabel={(option) => option.sessionYear}
      value={activeSession || null}
      onChange={handleChange}
      loading={loading || isSwitchingSession}
      isOptionEqualToValue={(option, value) => option.id === value.id}
      renderOption={(props, option) => (
        <li {...props} key={option.id}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
            <Typography variant="body2" sx={{ flex: 1 }}>
              {option.sessionYear}
            </Typography>
            {option.isActive && (
              <Chip label="Active" color="success" size="small" sx={{ height: 20, fontSize: 10 }} />
            )}
          </Box>
        </li>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder="Session"
          variant="outlined"
          sx={{
            minWidth: 140,
            '& .MuiOutlinedInput-root': {
              height: 32,
              fontSize: 12,
              '& fieldset': { borderColor: 'rgba(255,255,255,0.15)' },
              '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.3)' },
            },
          }}
        />
      )}
      sx={{
        width: 160,
        '& .MuiAutocomplete-inputRoot': {
          pr: '28px !important',
        },
      }}
    />
  );
}

export default SessionSelector;
