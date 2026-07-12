import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import Home from './pages/Home';
import Quizzes from './pages/Quizzes';
import QuizPage from './pages/QuizPage';
import Leaderboard from './pages/Leaderboard';
import Badges from './pages/Badges';
import './App.css';

export default function App() {
  return (
    <BrowserRouter>
      <nav className="navbar">
        <span className="brand">QuizQuest</span>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/quizzes">Quizzes</NavLink>
        <NavLink to="/leaderboard">Leaderboard</NavLink>
        <NavLink to="/badges">Badges</NavLink>
      </nav>
      <main className="container">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/quizzes" element={<Quizzes />} />
          <Route path="/quizzes/:id" element={<QuizPage />} />
          <Route path="/leaderboard" element={<Leaderboard />} />
          <Route path="/badges" element={<Badges />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}