import axiosClient from './axiosClient';
import type { LocationState, LocationDistrict, LocationCity, PinLookupResult } from '../types/location.types';

export async function getLocationStates(): Promise<LocationState[]> {
  const response = await axiosClient.get<LocationState[]>('/locations/states');
  return response.data;
}

export async function getLocationDistricts(state: string): Promise<LocationDistrict[]> {
  const response = await axiosClient.get<LocationDistrict[]>('/locations/districts', { params: { state } });
  return response.data;
}

export async function getLocationCities(state: string, district: string): Promise<LocationCity[]> {
  const response = await axiosClient.get<LocationCity[]>('/locations/cities', { params: { state, district } });
  return response.data;
}

export async function lookupPinCode(pinCode: string): Promise<PinLookupResult> {
  const response = await axiosClient.get<PinLookupResult>(`/locations/pin/${pinCode}`);
  return response.data;
}
