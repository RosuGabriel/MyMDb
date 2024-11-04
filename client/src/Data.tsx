import axios, { AxiosInstance } from "axios";
import { refreshAccessToken } from "./services/UserService";

export const API_URL = "https://mymdb.tplinkdns.com/";

export let axiosRefreshInstance: AxiosInstance = axios.create({
  baseURL: API_URL + "api/user/",
});

export let axiosInstance: AxiosInstance = axios.create({
  baseURL: API_URL + "api/",
});

export const setAxiosInterceptors = () => {
  // request interceptor
  axiosInstance.interceptors.request.use(
    async (config) => {
      const creditentialsString =
        localStorage.getItem("creditentials") ||
        sessionStorage.getItem("creditentials");
      if (creditentialsString) {
        const creditentials: Creditentials = JSON.parse(creditentialsString);

        const currentTime = Math.floor(Date.now() / 1000);
        if (creditentials.exp < currentTime + 600) {
          const newCredentials = await refreshAccessToken();
          config.headers.Authorization = `Bearer ${newCredentials.token}`;
        } else {
          config.headers.Authorization = `Bearer ${creditentials.token}`;
        }
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  // response interceptor
  axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      if (error.response.status === 401) {
        // Unauthorized
        try {
          const newCredentials = await refreshAccessToken();
          originalRequest.headers[
            "Authorization"
          ] = `Bearer ${newCredentials.token}`;
          return axiosInstance(originalRequest);
        } catch (refreshError) {
          console.error("Failed to refresh access token:", refreshError);
        }
      }
      return Promise.reject(error);
    }
  );
};

setAxiosInterceptors();

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

export interface Creditentials {
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
