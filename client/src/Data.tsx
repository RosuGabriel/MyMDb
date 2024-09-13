import axios, { AxiosInstance } from "axios";

export const API_URL = "https://api_address.com/";

export let axiosInstance: AxiosInstance = axios.create({
  baseURL: API_URL + "api/",
});

export const setAxiosInterceptors = () => {
  axiosInstance.interceptors.request.use(
    (config) => {
      const creditentialsString =
        localStorage.getItem("creditentials") ||
        sessionStorage.getItem("creditentials");
      if (creditentialsString) {
        const creditentials: Creditentials = JSON.parse(creditentialsString);
        config.headers.Authorization = `Bearer ${creditentials.token}`;
      }
      return config;
    },
    (error) => {
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
