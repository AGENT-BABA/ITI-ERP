import { useState, useEffect, useRef } from 'react';
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
import type { AcademicSession } from '../../../types/common.types';

export function SessionSelector() {
  const { user, switchSession, isSwitchingSession } = useAuth();
  const [sessions, setSessions] = useState<AcademicSession[]>([]);
  const [loading, setLoading] = useState(false);
  const autoSwitchAttempted = useRef(false);

  useEffect(() => {
    setLoading(true);
    autoSwitchAttempted.current = false;

    const fetchSessions = async () => {
      try {
        if (user?.role === 'TradeHead' && user?.tradeId) {
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
  }, [user?.role, user?.tradeId]);

  // Auto-select active session if user has no session set (legacy token / first login)
  useEffect(() => {
    if (autoSwitchAttempted.current || loading || sessions.length === 0) return;
    if (user?.academicSessionId) return;

    autoSwitchAttempted.current = true;
    const activeSession = sessions.find(s => s.isActive) || sessions[0];
    if (activeSession) {
      switchSession(activeSession.id);
    }
  }, [sessions, loading, user?.academicSessionId, switchSession]);

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
