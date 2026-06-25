import { useEffect, useState } from 'react';
import { api, type Quiz } from '../api';

const diffClass: Record<string, string> = {
  Easy: 'badge-easy', Medium: 'badge-medium', Hard: 'badge-hard',
};

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

  if (loading) return <div className="container"><p className="empty">Loading...</p></div>;
  if (error) return <div className="container"><p className="empty">{error}</p></div>;

  return (
    <div className="container">
      <div className="page-header"><h1>Quizzes</h1></div>
      {quizzes.length === 0 && <p className="empty">No quizzes yet.</p>}
      {quizzes.map(q => (
        <div key={q.id} className="card">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <h2 style={{ flex: 1 }}>{q.title}</h2>
            <span className={`badge ${diffClass[q.difficulty] ?? ''}`}>{q.difficulty}</span>
          </div>
          {q.description && <p style={{ color: 'var(--muted)', marginTop: '0.4rem', fontSize: '0.9rem' }}>{q.description}</p>}
          <div className="stat-row" style={{ marginTop: '0.6rem' }}>
            <span className="stat">Category: <span>{q.category?.name ?? '—'}</span></span>
          </div>
        </div>
      ))}
    </div>
  );
}
