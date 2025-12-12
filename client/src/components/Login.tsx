import React, { useState } from "react";
import { isAdmin, login } from "../services/UserService";
import "../App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";

const LoginForm: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [passwordVisible, setPasswordVisible] = useState(false);
  const [remember, setRemember] = useState(false);
  const navigate = useNavigate();

  document.title = "MyMDb - Login";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      await login(email, password, remember);
      navigate("/");
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
    <form className="login-form p-3" onSubmit={handleSubmit}>
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

          <a href="/mymdb/register" className="d-block mb-4">
            Don't have an account? Register here.
          </a>
        </div>
      </div>
    </form>
  );
};

export default LoginForm;
