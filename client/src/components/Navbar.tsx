import { useEffect, useState } from "react";
import { isAdmin, isAuthenticated, logout } from "../services/UserService";
import "bootstrap/dist/css/bootstrap.min.css";

function Navbar() {
  const [isLogged, setIsLogged] = useState(isAuthenticated());
  const [isAdminUser, setIsAdminUser] = useState(isAdmin());

  useEffect(() => {
    const checkAuth = () => {
      setIsLogged(isAuthenticated());
      setIsAdminUser(isAdmin());
    };

    checkAuth();

    window.addEventListener("storage", checkAuth);
    window.removeEventListener("authChange", checkAuth);

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.addEventListener("authChange", checkAuth);
    };
  }, []);

  return (
    <nav className="navbar p-3 d-flex justify-content-between align-items-center bg-dark">
      <a className="btn btn-warning ms-5" href="/">
        MyMDb
      </a>
      <div className="btn-group me-5">
        {isAdminUser && (
          <a className="btn btn-secondary" href="/create">
            Add Media
          </a>
        )}
        {isLogged ? (
          <>
            <a className="btn btn-secondary">Profile</a>
            <button className="btn btn-secondary" onClick={() => logout()}>
              Logout
            </button>
          </>
        ) : (
          <a className="btn btn-dark" href="/login">
            Login
          </a>
        )}
      </div>
    </nav>
  );
}

export default Navbar;
