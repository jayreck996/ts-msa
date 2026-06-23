import { useEffect, useState } from 'react';
import { api, type Quiz } from '../api';

export default function Quizzes() {
  const [quizzes, setQuizzes] = useState<Quiz[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    api.getQuizzes()
      .then(setQuizzes)
      .catch(() => setError('Failed to load quizzes'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>{error}</p>;

  return (
    <div>
      <h1>Quizzes</h1>
      {quizzes.length === 0 && <p>No quizzes yet.</p>}
      <ul>
        {quizzes.map(q => (
          <li key={q.id}>
            <strong>{q.title}</strong> — {q.difficulty} | {q.category?.name}
          </li>
        ))}
      </ul>
    </div>
  );
}
