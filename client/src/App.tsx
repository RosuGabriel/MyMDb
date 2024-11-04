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
            <Route path="/" element={<Navigate to="/media" />} />
            <Route path="*" element={<Navigate to="/media" />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            <Route
              path="/media"
              element={<ProtectedRoute component={ListMedia} />}
            />
            <Route
              path="/media/:id"
              element={<ProtectedRoute component={ShowMedia} />}
            />
            <Route
              path="/profile"
              element={<ProtectedRoute component={Profile} />}
            />
            <Route
              path="/create"
              element={<ProtectedRoute component={CreateMedia} />}
            />
            <Route
              path="/add-episode/:id"
              element={<ProtectedRoute component={AddEpisode} />}
            />
            <Route
              path="/add-review/:id"
              element={<ProtectedRoute component={AddReview} />}
            />
            <Route
              path="/add-attribute/:id"
              element={<ProtectedRoute component={AddAttribute} />}
            />
          </Routes>
        </div>
      </BrowserRouter>
    </div>
  );
};

export default App;
