import React, { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { CreateMedia, AddEpisode } from "./components/CreateMedia";
import EditMovie from "./components/EditMovie";
import EditSeries from "./components/EditSeries";
import EditEpisode from "./components/EditEpisode";
import OfflinePage from "./components/OfflinePage";
import ProtectedRoute from "./components/ProtectedRoute";
import ListMedia from "./components/ListMedia";
import AddReview from "./components/AddReview";
import Register from "./components/Register";
import Login from "./components/Login";
import ShowMedia from "./components/ShowMedia";
import Navbar from "./components/Navbar";
import Profile from "./components/Profile";
import AddAttribute from "./components/AddAttribute";
import EditAttribute from "./components/EditAttribute";
import ManageAttributes from "./components/ManageAttributes";
import ManageUsers from "./components/ManageUsers";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";

const App: React.FC = () => {
  const [navbarHeight, setNavbarHeight] = useState(0);

  useEffect(() => {
    const navbar = document.querySelector(".navbar");
    if (navbar) {
      setNavbarHeight(navbar.scrollHeight + 1);
    }

    const handleResize = () => {
      if (navbar) {
        setNavbarHeight(navbar.scrollHeight + 1);
      }
    };

    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  const [offlineType, setOfflineType] = useState<
    null | "no-internet" | "server-down"
  >(null);

  useEffect(() => {
    if (!navigator.onLine) {
      setOfflineType("no-internet");
      return;
    }

    fetch("/mymdb/api/user/health", { method: "HEAD" })
      .then((res) => {
        if (!res.ok && res.status !== 404 && res.status !== 401) {
          setOfflineType("server-down");
        }
      })
      .catch(() => setOfflineType("server-down"));
  }, []);

  if (offlineType) {
    return <OfflinePage type={offlineType} />;
  }

  return (
    <div className="App">
      <BrowserRouter>
        <Navbar />
        <div
          className="content px-0 px-sm-1 px-md-3 px-lg-5 px-xl-5"
          style={{ paddingTop: `${navbarHeight}px` }}
        >
          <Routes>
            <Route path="/" element={<Navigate to="/mymdb/media" />} />
            <Route path="/mymdb" element={<Navigate to="/mymdb/media" />} />
            <Route path="/mymdb/login" element={<Login />} />
            <Route path="/mymdb/register" element={<Register />} />

            <Route
              path="/mymdb/media"
              element={<ProtectedRoute component={ListMedia} />}
            />
            <Route
              path="/mymdb/media/:id"
              element={<ProtectedRoute component={ShowMedia} />}
            />
            <Route
              path="/mymdb/profile"
              element={<ProtectedRoute component={Profile} />}
            />
            <Route
              path="/mymdb/create"
              element={<ProtectedRoute component={CreateMedia} />}
            />
            <Route
              path="/mymdb/add-episode/:id"
              element={<ProtectedRoute component={AddEpisode} />}
            />
            <Route
              path="/mymdb/add-review/:id"
              element={<ProtectedRoute component={AddReview} />}
            />
            <Route
              path="/mymdb/add-attribute/:id"
              element={<ProtectedRoute component={AddAttribute} />}
            />
            <Route
              path="/mymdb/manage-attributes/:id"
              element={<ProtectedRoute component={ManageAttributes} />}
            />
            <Route
              path="/mymdb/edit-attribute/:id"
              element={<ProtectedRoute component={EditAttribute} />}
            />
            <Route
              path="/mymdb/edit-movie/:id"
              element={<ProtectedRoute component={EditMovie} />}
            />
            <Route
              path="/mymdb/edit-series/:id"
              element={<ProtectedRoute component={EditSeries} />}
            />
            <Route
              path="/mymdb/edit-episode/:id"
              element={<ProtectedRoute component={EditEpisode} />}
            />
            <Route
              path="/mymdb/manage-users"
              element={<ProtectedRoute component={ManageUsers} />}
            />
          </Routes>
        </div>
      </BrowserRouter>
    </div>
  );
};

export default App;
