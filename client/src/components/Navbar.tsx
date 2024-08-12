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
    window.addEventListener("authChange", checkAuth);

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.removeEventListener("authChange", checkAuth);
    };
  }, []);

  return (
    <nav className="navbar navbar-expand-sm bg-dark navbar-dark fixed-top d-flex p-3 justify-content-between align-items-center">
      <a className="btn btn-warning" href="/" style={{ fontWeight: "900" }}>
        MyMDb
      </a>
      <div className="btn-group">
        {isAdminUser && (
          <a className="btn btn-secondary" href="/create">
            Add Media
          </a>
        )}
        {isLogged ? (
          <>
            <a className="btn btn-secondary" href="/profile">
              Profile
            </a>
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
