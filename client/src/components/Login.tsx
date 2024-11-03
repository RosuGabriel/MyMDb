import React, { useState, useEffect } from "react";
import { isAdmin, isAuthenticated, login } from "../services/UserService";
import "../App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate, useLocation } from "react-router-dom";

const LoginForm: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [passwordVisible, setPasswordVisible] = useState(false);
  const [loggedIn, setLoggedIn] = useState(isAuthenticated());
  const [remember, setRemember] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  document.title = "MyMDb - Login";
  console.log("auth " + isAuthenticated());
  console.log("logged " + loggedIn);

  useEffect(() => {
    if (isAuthenticated()) {
      if (location.state?.from || document.referrer) {
        const previousPage = location.state?.from || document.referrer;
        const isExternalSite =
          document.referrer &&
          new URL(document.referrer).origin !== window.location.origin;
        const isRegisterPage = previousPage?.includes("/register");

        if (isExternalSite || isRegisterPage) {
          navigate("/", { replace: true });
        } else {
          navigate(-1);
        }
      } else {
        navigate(-1);
      }
    }
  }, [loggedIn]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      await login(email, password, remember);
      console.log("User logged in.");
      if (isAdmin()) {
        console.log("User is admin.");
      }
      setLoggedIn(true);
    } catch (error: any) {
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError("An unexpected error occurred.");
      }
      console.error("Error logging in:", error);
    }
  };

  const togglePasswordVisibility = () => {
    setPasswordVisible(!passwordVisible);
  };

  const toggleRemember = () => {
    setRemember(!remember);
  };

  return (
    <form className="login-form" onSubmit={handleSubmit}>
      <br />
      <div className="mb-3">
        <label htmlFor="email" className="form-label">
          Email
        </label>
        <input
          className="form-control"
          type="email"
          id="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="name@example.com"
          required
        />
      </div>
      <div className="mb-3">
        <label htmlFor="password" className="form-label">
          Password
        </label>
        <div className="input-group">
          <input
            className="form-control"
            type={passwordVisible ? "text" : "password"}
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <button
            className="btn btn-outline-secondary"
            type="button"
            onClick={togglePasswordVisibility}
            aria-label={passwordVisible ? "Hide password" : "Show password"}
          >
            {passwordVisible ? "🙉" : "🙈"}
          </button>
        </div>
        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}
      </div>
      <div className="container mt-5">
        <div className="d-flex flex-column align-items-center text-center">
          <div className="login-section d-flex align-items-center">
            <button type="submit" className="btn btn-success rounded mb-4">
              Login
            </button>
            <div className="d-flex align-items-center justify-content-center mb-4 ms-3">
              <input
                className="form-check-input"
                type="checkbox"
                id="checkRemember"
                checked={remember}
                onChange={toggleRemember}
              />
              <label className="form-check-label ms-2" htmlFor="checkRemember">
                Remember me
              </label>
            </div>
          </div>

          <a href="/register" className="d-block mb-4">
            Don't have an account? Register here.
          </a>
        </div>
      </div>
    </form>
  );
};

export default LoginForm;
