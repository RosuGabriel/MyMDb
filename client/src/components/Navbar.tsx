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
  const [navbarWidth, setNavbarWidth] = useState(900);

  const handleToggle = () => {
    setIsCollapsed(!isCollapsed);
  };

  useEffect(() => {
    const checkAuth = () => {
      setIsLogged(isAuthenticated());
      setIsAdminUser(isAdmin());
    };

    const handleResize = () => {
      setNavbarWidth(document.querySelector("#navbarNav")?.clientWidth || 0);
    };

    setNavbarWidth(document.querySelector("#navbarNav")?.clientWidth || 0);

    setSearch(new URLSearchParams(location.search).get("search") || "");
    window.addEventListener("storage", checkAuth);
    window.addEventListener("authChange", checkAuth);
    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.removeEventListener("authChange", checkAuth);
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  const handleSearch = (e: any) => {
    e.preventDefault();
    const searchParams = new URLSearchParams(location.search);
    searchParams.set("search", searchQuery);
    if (!searchQuery.trim()) {
      searchParams.delete("search");
    }
    navigate({ pathname: "/media", search: searchParams.toString() });
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark fixed-top p-3">
      <div className="container-fluid d-flex align-items-center">
        <a
          className="navbar-brand bg-warning px-2 rounded text-dark"
          href="/"
          style={{ fontWeight: "900" }}
        >
          MyMDb
        </a>

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
                className: `searchbar d-flex input-group ${
                  !isCollapsed && "mb-2"
                }`,
                style: isCollapsed
                  ? { marginLeft: (navbarWidth - 411) / 2 + "px" }
                  : { margin: "auto" },
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
                  <button className="btn btn-secondary" onClick={logout}>
                    Logout
                  </button>
                </>
              ) : (
                <a className="btn btn-dark" href="/login">
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
