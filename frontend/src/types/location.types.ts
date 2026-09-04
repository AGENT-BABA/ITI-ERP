export interface LocationState {
  name: string;
}

export interface LocationDistrict {
  name: string;
}

export interface LocationCity {
  name: string;
}

export interface PostOffice {
  name: string;
  district: string;
  state: string;
  division?: string;
  region?: string;
  taluk?: string;
}

export interface PinLookupResult {
  pinCode: string;
  state: string;
  district: string;
  postOffices: PostOffice[];
}
