import { jwtDecode } from "jwt-decode";
import axios from "axios";
import { Credentials, apiClient, UserProfile, refreshApiClient } from "../Data";

export const register = async (email: string, password: string) => {
  const user = { email, password };
  try {
    const response = await apiClient.post("user/register", user, {
      headers: {
        "Content-Type": "application/json",
      },
    });
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error("Error registering user:", error.response?.data);
    } else {
      console.error("Unexpected error:", error);
    }
    throw error;
  }
};

let lastStorage: "local" | "session" = "local";

export const refreshAccessToken = async (): Promise<Credentials> => {
  const credentials = getCredentials();
  if (!credentials) throw new Error("No credentials found.");

  try {
    const response = await refreshApiClient.post("refresh", {
      token: credentials.token,
      refreshToken: credentials.refreshToken,
    });

    const newCreds = decodeToken(response.data.token);
    newCreds.token = response.data.token;
    newCreds.refreshToken = response.data.refreshToken;

    if (lastStorage === "local") saveTokenLocal(newCreds);
    else saveTokenSession(newCreds);

    window.dispatchEvent(new Event("authChange"));
    console.log("Access token refreshed.");
    return newCreds;
  } catch (error: any) {
    if (!error.response) {
      console.warn("Server offline!");
      throw error;
    }

    if (error.response.status === 401 || error.response.status === 403) {
      console.error(
        "Something went wrong with your session. Please log in again.",
        error.response.status
      );
      logout();
      window.location.href = "/mymdb/login";
      throw error;
    }

    console.error("Refresh failed!", error.response.status);
    throw error;
  }
};

export const login = async (
  email: string,
  password: string,
  remember: boolean
) => {
  try {
    const response = await apiClient.post("user/login", { email, password });
    const credentials = decodeToken(response.data.token);
    credentials.token = response.data.token;
    credentials.refreshToken = response.data.refreshToken;

    lastStorage = remember ? "local" : "session";
    if (remember) saveTokenLocal(credentials);
    else saveTokenSession(credentials);

    window.dispatchEvent(new Event("authChange"));
    console.log("User logged in:", credentials);
  } catch (error) {
    console.error("Error logging in user:", error);
    throw error;
  }
};

export const logout = () => {
  console.log("Logging out user.");
  localStorage.removeItem("credentials");
  sessionStorage.removeItem("credentials");
  window.dispatchEvent(new Event("authChange"));
  console.log("User logged out.");
};

export const getCredentials = (): Credentials | null => {
  let credentials = localStorage.getItem("credentials");
  if (!credentials) {
    credentials = sessionStorage.getItem("credentials");
  }
  if (!credentials) {
    return null;
  }
  return JSON.parse(credentials) as Credentials;
};

export const getLoggedUser = (): string | null => {
  return getCredentials()?.nameid!;
};

export const isAuthenticated = (): boolean => {
  if (getCredentials() == null) {
    return false;
  }
  return true;
};

export const isAdmin = (): boolean | null => {
  const credentials = getCredentials();
  return credentials && credentials.role.includes("admin");
};

const decodeToken = (token: string): Credentials => {
  const decodedToken = jwtDecode(token);
  return decodedToken as Credentials;
};

const saveTokenLocal = (credentials: Credentials) => {
  localStorage.setItem("credentials", JSON.stringify(credentials));
};

const saveTokenSession = (credentials: Credentials) => {
  sessionStorage.setItem("credentials", JSON.stringify(credentials));
};

export const fetchProfile = async (): Promise<UserProfile> => {
  try {
    const response = await apiClient.get("user/profile");
    return response.data;
  } catch (error) {
    console.error("Error fetching user profile:", error);
    throw error;
  }
};

export const updateProfile = async (formData: FormData) => {
  try {
    const response = await apiClient.post("user/edit_profile", formData);
    return response.data;
  } catch (error) {
    console.error("Error updating user profile:", error);
    throw error;
  }
};
