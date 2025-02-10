import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { isAdmin, isAuthenticated, logout } from "../services/UserService";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";

function Navbar() {
  const [isLogged, setIsLogged] = useState(isAuthenticated());
  const [isAdminUser, setIsAdminUser] = useState(isAdmin());
  const navigate = useNavigate();
  const [searchQuery, setSearch] = useState("");
  const [isCollapsed, setIsCollapsed] = useState(true);
  const [isLargeScreen, setIsLargeScreen] = useState(window.innerWidth >= 992);
  const [logoWidth, setLogoWidth] = useState(0);
  const [userButtonsWidth, setUserButtonsWidth] = useState(0);

  const handleToggle = () => {
    setIsCollapsed(!isCollapsed);
  };

  const handleResize = () => {
    setIsLargeScreen(window.innerWidth >= 992);
  };

  useEffect(() => {
    setSearch(new URLSearchParams(location.search).get("search") || "");

    const checkAuth = () => {
      setIsLogged(isAuthenticated());
      setIsAdminUser(isAdmin());
    };

    checkAuth();
    window.addEventListener("storage", checkAuth);
    window.addEventListener("authChange", checkAuth);
    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.removeEventListener("authChange", checkAuth);
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  useEffect(() => {
    const userButtons = document.getElementById("user-buttons");
    const logoButton = document.getElementById("app-logo");
    if (userButtons && logoButton) {
      setUserButtonsWidth(userButtons.offsetWidth);
      setLogoWidth(logoButton.offsetWidth);
    }
  }, [isLogged, isAdminUser]);

  const handleSearch = (e: any) => {
    e.preventDefault();
    const searchParams = new URLSearchParams(location.search);
    searchParams.set("search", searchQuery);
    if (!searchQuery.trim()) {
      searchParams.delete("search");
    }
    navigate({ pathname: "/mymdb/media", search: searchParams.toString() });
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark fixed-top p-3">
      <div className="container-fluid d-flex align-items-center">
        <a
          id="app-logo"
          className="navbar-brand bg-warning px-2 rounded text-dark"
          href="/"
          style={{
            fontWeight: "900",
            marginRight:
              isLogged && isLargeScreen
                ? userButtonsWidth - logoWidth + "px"
                : "0px",
          }}
        >
          MyMDb
        </a>
        {/* "154.77px" */}
        <button
          className="navbar-toggler"
          type="button"
          onClick={handleToggle}
          aria-controls="navbarNav"
          aria-expanded={!isCollapsed}
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div
          className={`collapse navbar-collapse ${isCollapsed ? "" : "show"}`}
          id="navbarNav"
        >
          <div
            {...(isCollapsed
              ? {
                  className:
                    "d-flex justify-content-between w-100 align-items-center",
                }
              : {
                  className:
                    "d-flex flex-column align-items-center justify-content-center pt-3",
                })}
          >
            <form
              {...{
                className: `searchbar d-flex input-group mx-auto ${
                  !isCollapsed && "mb-2"
                }`,
              }}
              onSubmit={handleSearch}
            >
              <input
                className="form-control bg-dark text-white border-secondary"
                type="search"
                placeholder="Search"
                aria-label="Search"
                value={searchQuery}
                onChange={(e) => setSearch(e.target.value)}
              />
              <button className="btn btn-outline-secondary" type="submit">
                🔎
              </button>
            </form>

            <div id="user-buttons" className="btn-group">
              {isAdminUser && (
                <a className="btn btn-dark" href="/mymdb/create">
                  Add Media
                </a>
              )}
              {isLogged ? (
                <>
                  <a className="btn btn-dark" href="/mymdb/profile">
                    Profile
                  </a>
                  <a
                    className="btn btn-dark"
                    onClick={logout}
                    href="/mymdb/login"
                  >
                    Logout
                  </a>
                </>
              ) : (
                <a
                  className="btn btn-dark"
                  href="/mymdb/login"
                  style={isCollapsed ? { marginLeft: "30.12px" } : {}}
                >
                  Login
                </a>
              )}
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
