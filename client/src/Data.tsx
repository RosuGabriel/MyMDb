import axios, { AxiosInstance } from "axios";
import { refreshAccessToken, getCredentials } from "./services/UserService";

// API Clients

export const API_URL = import.meta.env.VITE_API_URL;

export let refreshApiClient: AxiosInstance = axios.create({
  baseURL: API_URL + "api/user/",
});

export let apiClient: AxiosInstance = axios.create({
  baseURL: API_URL + "api/",
});

export let staticClient: AxiosInstance = axios.create({
  baseURL: API_URL + "static/",
});

let refreshingPromise: Promise<any> | null = null;

export const setAxiosInterceptors = () => {
  // Prevent multiple interceptor setups
  if ((apiClient as any)._interceptorsSet) return;
  (apiClient as any)._interceptorsSet = true;
  (staticClient as any)._interceptorsSet = true;

  const attachAuthHeader = async (config: any, bufferTime = 300) => {
    const credentials = getCredentials();
    if (!credentials) return config;

    const currentTime = Math.floor(Date.now() / 1000);
    if (credentials.exp < currentTime + bufferTime) {
      // Wait if a refresh is already in progress
      if (!refreshingPromise) {
        refreshingPromise = refreshAccessToken();
      }
      const newCreds = await refreshingPromise;
      refreshingPromise = null;
      config.headers.Authorization = `Bearer ${newCreds.token}`;
    } else {
      config.headers.Authorization = `Bearer ${credentials.token}`;
    }
    return config;
  };

  // Request interceptors
  apiClient.interceptors.request.use((config) => attachAuthHeader(config, 300));
  staticClient.interceptors.request.use((config) =>
    attachAuthHeader(config, 100)
  );

  // Response interceptors
  apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      if (error.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;

        try {
          if (!refreshingPromise) refreshingPromise = refreshAccessToken();
          const newCreds = await refreshingPromise;
          refreshingPromise = null;
          originalRequest.headers["Authorization"] = `Bearer ${newCreds.token}`;
          return apiClient(originalRequest);
        } catch (err) {
          refreshingPromise = null;
          // Token refresh failed, logout
          return Promise.reject(err);
        }
      }
      return Promise.reject(error);
    }
  );
};

setAxiosInterceptors();

// Data Interfaces

export interface Media {
  id: string;
  dateCreated: Date;
  dateModified: Date;
  title: string;
  description: string;
  mediaType: "Movie" | "Series" | "Episode";
  releaseDate?: Date;
  posterPath: string;
  videoPath: string;
  episodeNumber: number;
  seasonNumber: number;
  seasons: number;
  series: Media;
  episodes: { $values: Media[] };
  seriesId: string;
  reviews: { $values: Review[] };
  mediaAttributes: { $values: Attribute[] };
}

export interface Review {
  id: string;
  userProfile: UserProfile;
  userId: string;
  mediaId: string;
  createDate: Date;
  modifiedDate: Date;
  rating: number;
  comment: string;
}

export interface Credentials {
  nameid: string;
  email: string;
  role: string;
  exp: number;
  token: string;
  refreshToken: string;
}

export interface UserProfile {
  userId: string;
  userName: string;
  profilePicPath: string;
}

export interface Attribute {
  mediaId: string;
  attributePath: string;
  type: string;
  language: string;
}

export const Languages = [
  "English",
  "Română",
  "Français",
  "Deutsch",
  "Español",
];

export interface IContinueWatching {
  mediaId: string;
  episodeId: string;
  watchedTime: number;
  duration: number;
  episodeNumber: number;
  seasonNumber: number;
  posterPath: string;
}
