import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSnackbar } from 'notistack';
import {
  Stack,
  TextField,
  Autocomplete,
  Dialog,
  DialogTitle,
  DialogContent,
  List,
  ListItemButton,
  ListItemText,
} from '@mui/material';
import {
  getLocationStates,
  getLocationDistricts,
  getLocationCities,
  lookupPinCode,
} from '../../api/location.api';
import type { PostOffice } from '../../types/location.types';

interface LocationSelectorProps {
  state: string;
  district: string;
  city: string;
  pinCode: string;
  onStateChange: (value: string) => void;
  onDistrictChange: (value: string) => void;
  onCityChange: (value: string) => void;
  onPinChange: (value: string) => void;
  showPinCode?: boolean;
}

export default function LocationSelector({
  state,
  district,
  city,
  pinCode,
  onStateChange,
  onDistrictChange,
  onCityChange,
  onPinChange,
  showPinCode = false,
}: LocationSelectorProps) {
  const { enqueueSnackbar } = useSnackbar();
  const [postOfficeDialogOpen, setPostOfficeDialogOpen] = useState(false);
  const [pendingPostOffices, setPendingPostOffices] = useState<PostOffice[]>([]);
  const [pendingPinState, setPendingPinState] = useState('');
  const [pendingPinDistrict, setPendingPinDistrict] = useState('');

  const { data: states = [], isLoading: loadingStates } = useQuery({
    queryKey: ['locationStates'],
    queryFn: getLocationStates,
  });

  const { data: districts = [], isLoading: loadingDistricts } = useQuery({
    queryKey: ['locationDistricts', state],
    queryFn: () => getLocationDistricts(state),
    enabled: Boolean(state),
  });

  const { data: cities = [], isLoading: loadingCities } = useQuery({
    queryKey: ['locationCities', state, district],
    queryFn: () => getLocationCities(state, district),
    enabled: Boolean(state && district),
  });

  const stateOptions = useMemo(() => states.map((s) => s.name), [states]);
  const districtOptions = useMemo(() => districts.map((d) => d.name), [districts]);
  const cityOptions = useMemo(() => cities.map((c) => c.name), [cities]);

  const handlePinBlur = async () => {
    const trimmed = pinCode.trim();
    if (trimmed.length !== 6 || !/^\d{6}$/.test(trimmed)) return;

    try {
      const result = await lookupPinCode(trimmed);

      if (!result || result.postOffices.length === 0) {
        enqueueSnackbar(`PIN code ${trimmed} not found`, { variant: 'warning' });
        return;
      }

      if (result.postOffices.length === 1) {
        const po = result.postOffices[0];
        if (!state) onStateChange(result.state || po.state);
        if (!district) onDistrictChange(result.district || po.district);
        if (!city) onCityChange(po.name);
      } else if (result.postOffices.length > 1) {
        setPendingPinState(result.state);
        setPendingPinDistrict(result.district);
        setPendingPostOffices(result.postOffices);
        setPostOfficeDialogOpen(true);
      }
    } catch {
      enqueueSnackbar(`PIN code ${trimmed} not found`, { variant: 'error' });
    }
  };

  const handlePostOfficeSelect = (po: PostOffice) => {
    if (!state) onStateChange(pendingPinState || po.state);
    if (!district) onDistrictChange(pendingPinDistrict || po.district);
    if (!city) onCityChange(po.name);
    setPostOfficeDialogOpen(false);
    setPendingPostOffices([]);
  };

  return (
    <>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        {showPinCode && (
          <TextField
            label="Pin Code"
            value={pinCode}
            onChange={(e) => {
              const val = e.target.value.replace(/\D/g, '').slice(0, 6);
              onPinChange(val);
            }}
            onBlur={handlePinBlur}
            fullWidth
            slotProps={{ htmlInput: { maxLength: 6, inputMode: 'numeric' } }}
          />
        )}

        <Autocomplete
          freeSolo
          options={stateOptions}
          loading={loadingStates}
          value={state}
          onChange={(_, newValue) => {
            onStateChange(typeof newValue === 'string' ? newValue : newValue || '');
            onDistrictChange('');
            onCityChange('');
          }}
          renderInput={(params) => <TextField {...params} label="State" fullWidth />}
          fullWidth
        />

        <Autocomplete
          freeSolo
          options={districtOptions}
          loading={loadingDistricts}
          value={district}
          onChange={(_, newValue) => {
            onDistrictChange(typeof newValue === 'string' ? newValue : newValue || '');
            onCityChange('');
          }}
          renderInput={(params) => <TextField {...params} label="District" fullWidth disabled={!state} />}
          fullWidth
        />

        <Autocomplete
          freeSolo
          options={cityOptions}
          loading={loadingCities}
          value={city}
          onChange={(_, newValue) => {
            onCityChange(typeof newValue === 'string' ? newValue : newValue || '');
          }}
          renderInput={(params) => <TextField {...params} label="City" fullWidth disabled={!district} />}
          fullWidth
        />
      </Stack>

      <Dialog
        open={postOfficeDialogOpen}
        onClose={() => setPostOfficeDialogOpen(false)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle>Select Post Office</DialogTitle>
        <DialogContent dividers>
          <List dense>
            {pendingPostOffices.map((po) => (
              <ListItemButton key={po.name} onClick={() => handlePostOfficeSelect(po)}>
                <ListItemText primary={po.name} secondary={`${po.district}, ${po.state}`} />
              </ListItemButton>
            ))}
          </List>
        </DialogContent>
      </Dialog>
    </>
  );
}
