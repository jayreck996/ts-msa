import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import Home from './pages/Home';
import Quizzes from './pages/Quizzes';
import QuizPage from './pages/QuizPage';
import Leaderboard from './pages/Leaderboard';
import Badges from './pages/Badges';
import Auth from './pages/Auth';
import { useAuthStore } from './store/authStore';
import { ThemeToggle } from './ThemeToggle';
import './App.css';

function AuthNav() {
  const username = useAuthStore(s => s.username);
  const logout = useAuthStore(s => s.logout);
  return username
    ? <span className="stat">{username} <button type="button" className="btn-outline" style={{ padding: '0.2rem 0.6rem' }} onClick={logout}>Log Out</button></span>
    : <NavLink to="/login">Log In</NavLink>;
}

export default function App() {
  return (
    <BrowserRouter>
      <nav className="navbar">
        <span className="brand">QuizQuest</span>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/quizzes">Quizzes</NavLink>
        <NavLink to="/leaderboard">Leaderboard</NavLink>
        <NavLink to="/badges">Badges</NavLink>
        <AuthNav />
        <ThemeToggle />
      </nav>
      <main className="container">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/quizzes" element={<Quizzes />} />
          <Route path="/quizzes/:id" element={<QuizPage />} />
          <Route path="/leaderboard" element={<Leaderboard />} />
          <Route path="/badges" element={<Badges />} />
          <Route path="/login" element={<Auth />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}