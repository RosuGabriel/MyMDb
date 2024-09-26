import React, { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { CreateMedia, AddEpisode } from "./components/CreateMedia";
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
    const navbar = document.querySelector(".navbar"); // Selectorul pentru navbar
    if (navbar) {
      setNavbarHeight(navbar.scrollHeight + 1); // Obține înălțimea și adaugă 1 pixel
    }

    const handleResize = () => {
      if (navbar) {
        setNavbarHeight(navbar.scrollHeight + 1); // Actualizează înălțimea la redimensionare
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
            <Route path="/media" element={<ListMedia />} />
            <Route path="/media/:id" element={<ShowMedia />} />
            <Route path="/create" element={<CreateMedia />} />
            <Route path="/add-episode/:id" element={<AddEpisode />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/add-review/:id" element={<AddReview />} />
            <Route path="/add-attribute/:id" element={<AddAttribute />} />
          </Routes>
        </div>
      </BrowserRouter>
    </div>
  );
};

export default App;
