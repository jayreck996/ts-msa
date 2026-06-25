import { Link } from 'react-router-dom';

export default function Home() {
  return (
    <div className="hero">
      <h1>QuizQuest</h1>
      <p>Test your knowledge, earn points, climb the leaderboard.</p>
      <div className="hero-links">
        <Link to="/quizzes" className="btn-primary">Browse Quizzes</Link>
        <Link to="/leaderboard" className="btn-outline">Leaderboard</Link>
        <Link to="/badges" className="btn-outline">Badges</Link>
      </div>
    </div>
  );
}
