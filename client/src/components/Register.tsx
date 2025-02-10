import React, { useState } from "react";
import { register } from "../services/UserService";
import "../App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";

const RegisterForm: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [passwordVisible, setPasswordVisible] = useState(false);
  const navigate = useNavigate();
  document.title = "MyMDb - Register";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      await register(email, password);
      console.log("User registered.");
      navigate("/mymdb/login");
    } catch (error: any) {
      if (error.response && error.response.data && error.response.data.Errors) {
        const keys = Object.keys(error.response.data.Errors);
        setError(error.response.data.Errors[keys[0]]);
      } else {
        setError("An unexpected error occurred.");
      }
      console.error("Error registering user:", error);
    }
  };

  const togglePasswordVisibility = () => {
    setPasswordVisible(!passwordVisible);
  };

  return (
    <form className="register-form" onSubmit={handleSubmit}>
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
          >
            {passwordVisible ? "🙉" : "🙈"}
          </button>
        </div>
        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}
        <div className="form-text text-warning">
          Your password must be at least 6 characters long, contain letters
          upper and lowercase, numbers, and non-alphanumeric character.
        </div>
      </div>
      <button type="submit" className="btn btn-success">
        Register
      </button>
    </form>
  );
};

export default RegisterForm;
