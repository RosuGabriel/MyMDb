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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      await login(email, password, remember);
      console.log("User logged in.");
      if (isAdmin()) {
        console.log("User is admin.");
      }
      navigate("/");
    } catch (error: any) {
      if (
        error.response &&
        error.response.data &&
        error.response.data.message
      ) {
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
      <div className="input-group d-flex justify-content-between align-items-center">
        <button type="submit" className="btn btn-success rounded">
          Login
        </button>
        <a href="/register">Don't have an account? Register here.</a>

        <div className="form-check">
          <input
            className="form-check-input"
            type="checkbox"
            id="checkRemember"
            checked={remember}
            onChange={toggleRemember}
          />
          <label className="form-check-label ms-1" htmlFor="checkRemember">
            Remember me
          </label>
        </div>
      </div>
    </form>
  );
};

export default LoginForm;
