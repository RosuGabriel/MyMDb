import React, { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { CreateMedia, AddEpisode } from "./components/CreateMedia";
import ProtectedRoute from "./components/ProtectedRoute";
import ListMedia from "./components/ListMedia";
import AddReview from "./components/AddReview";
import Register from "./components/Register";
import Login from "./components/Login";
import ShowMedia from "./components/ShowMedia";
import Navbar from "./components/Navbar";
import Profile from "./components/Profile";
import AddAttribute from "./components/AddAttribute";
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
          </Routes>
        </div>
      </BrowserRouter>
    </div>
  );
};

export default App;
