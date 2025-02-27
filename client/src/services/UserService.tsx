import { jwtDecode } from "jwt-decode";
import axios from "axios";
import {
  Creditentials,
  apiClient,
  setAxiosInterceptors,
  UserProfile,
  refreshApiClient,
} from "../Data";

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

export const login = async (
  email: string,
  password: string,
  remember: boolean
) => {
  try {
    const response = await apiClient.post("user/login", {
      email,
      password,
    });
    const creditentials = decodeToken(response.data.token);
    creditentials.token = response.data.token;
    creditentials.refreshToken = response.data.refreshToken;
    if (remember) {
      saveTokenLocal(creditentials);
    } else {
      saveTokenSession(creditentials);
    }
    window.dispatchEvent(new Event("authChange"));
    console.log("User logged in:", creditentials);
    setAxiosInterceptors();
  } catch (error) {
    console.error("Error logging in user:", error);
    throw error;
  }
};

export const refreshAccessToken = async (): Promise<Creditentials> => {
  while (localStorage.getItem("refreshing")) {
    await new Promise((resolve) => setTimeout(resolve, 500));
  }
  localStorage.setItem("refreshing", "x");

  const creditentials = getCreditentials();
  if (!creditentials) {
    throw new Error("No creditentials found.");
  }

  const token = creditentials.token;
  const refreshToken = creditentials.refreshToken;

  try {
    const response = await refreshApiClient.post("refresh", {
      token: token,
      refreshToken: refreshToken,
    });
    const creditentials = decodeToken(response.data.token);
    creditentials.token = response.data.token;
    creditentials.refreshToken = response.data.refreshToken;
    saveTokenLocal(creditentials);
    window.dispatchEvent(new Event("authChange"));
    localStorage.removeItem("refreshing");
    return creditentials;
  } catch (error) {
    console.error("Error refreshing token:", error);
    logout();
    window.location.href = "/mymdb/login";
    localStorage.removeItem("refreshing");
    throw error;
  }
};

export const logout = () => {
  localStorage.removeItem("creditentials");
  sessionStorage.removeItem("creditentials");
  window.dispatchEvent(new Event("authChange"));
  console.log("User logged out.");
  setAxiosInterceptors();
};

const getCreditentials = (): Creditentials | null => {
  let creditentials = localStorage.getItem("creditentials");
  if (!creditentials) {
    creditentials = sessionStorage.getItem("creditentials");
  }
  if (!creditentials) {
    return null;
  }
  return JSON.parse(creditentials) as Creditentials;
};

export const getLoggedUser = (): string | null => {
  return getCreditentials()?.nameid!;
};

export const isAuthenticated = (): boolean => {
  if (getCreditentials() == null) {
    return false;
  }
  return true;
};

export const isAdmin = (): boolean | null => {
  const creditentials = getCreditentials();
  return creditentials && creditentials.role.includes("admin");
};

const decodeToken = (token: string): Creditentials => {
  const decodedToken = jwtDecode(token);
  return decodedToken as Creditentials;
};

const saveTokenLocal = (creditentials: Creditentials) => {
  localStorage.setItem("creditentials", JSON.stringify(creditentials));
};

const saveTokenSession = (creditentials: Creditentials) => {
  sessionStorage.setItem("creditentials", JSON.stringify(creditentials));
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
