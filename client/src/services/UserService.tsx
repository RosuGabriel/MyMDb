import { jwtDecode } from "jwt-decode";
import axios from "axios";
import { Creditentials, axiosInstance, setAxiosInterceptors } from "../Data";

export const register = async (email: string, password: string) => {
  const user = { email, password };
  try {
    const response = await axiosInstance.post("user/register", user, {
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
    const response = await axiosInstance.post("user/login", {
      email,
      password,
    });
    const creditentials = decodeToken(response.data.token);
    creditentials.token = response.data.token;
    if (remember) {
      saveTokenLocal(creditentials);
    } else {
      savetokenSession(creditentials);
    }
    window.dispatchEvent(new Event("authChange"));
    console.log("User logged in:", creditentials);
    setAxiosInterceptors();
  } catch (error) {
    console.error("Error logging in user:", error);
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

const savetokenSession = (creditentials: Creditentials) => {
  sessionStorage.setItem("creditentials", JSON.stringify(creditentials));
};
