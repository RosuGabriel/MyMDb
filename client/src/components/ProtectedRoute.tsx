import React from "react";
import { Navigate } from "react-router-dom";
import { isAuthenticated } from "../services/UserService";

interface ProtectedRouteProps {
  component: React.ComponentType<any>;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  component: Component,
}) => {
  const isLogged = isAuthenticated();
  return isLogged ? <Component /> : <Navigate to="/mymdb/login" replace />;
};

export default ProtectedRoute;
