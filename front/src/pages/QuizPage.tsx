import { useEffect, useState } from 'react';
import { useParams, Link, Navigate } from 'react-router-dom';
import { api, type Quiz, type Question } from '../api';
import { useAuthStore } from '../store/authStore';

const diffClass: Record<string, string> = {
  Easy: 'badge-easy', Medium: 'badge-medium', Hard: 'badge-hard',
};

interface Result { score: number; total: number; pointsEarned: number; }

export default function QuizPage() {
  const { id } = useParams();
  const quizId = Number(id);
  const token = useAuthStore(s => s.token);

  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [answers, setAnswers] = useState<Record<number, number>>({}); // questionId -> optionId
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<Result | null>(null);

  useEffect(() => {
    if (!quizId) { setError('Invalid quiz'); setLoading(false); return; }
    Promise.all([api.getQuiz(quizId), api.getQuestions(quizId)])
      .then(([q, qs]) => { setQuiz(q); setQuestions(qs); })
      .catch(() => setError('Failed to load quiz'))
      .finally(() => setLoading(false));
  }, [quizId]);

  const select = (questionId: number, optionId: number) =>
    setAnswers(prev => ({ ...prev, [questionId]: optionId }));

  const allAnswered = questions.length > 0 && questions.every(q => answers[q.id] != null);

  async function submit() {
    setSubmitting(true);
    const score = questions.reduce(
      (n, q) => n + (answers[q.id] === q.correctOptionId ? 1 : 0), 0);
    try {
      const attempt = await api.submitAttempt({
        quizId,
        score,
        completedAt: new Date().toISOString(),
      });
      setResult({ score, total: questions.length, pointsEarned: attempt.pointsEarned });
    } catch {
      setError('Failed to submit attempt');
    } finally {
      setSubmitting(false);
    }
  }

  if (!token) return <Navigate to="/login" replace />;
  if (loading) return <div className="container"><p className="empty">Loading...</p></div>;
  if (error) return <div className="container"><p className="empty">{error}</p></div>;
  if (!quiz) return <div className="container"><p className="empty">Quiz not found.</p></div>;

  if (result) {
    const perfect = result.score === result.total;
    return (
      <div className="container">
        <div className="page-header"><h1>{quiz.title} — Results</h1></div>
        <div className="card" style={{ textAlign: 'center' }}>
          <p style={{ fontSize: '2.5rem', margin: '0.2rem 0' }}>{perfect ? '🏆' : '✅'}</p>
          <h2 style={{ margin: '0.2rem 0' }}>{result.score} / {result.total} correct</h2>
          <p style={{ color: 'var(--muted)', marginTop: '0.3rem' }}>
            +{result.pointsEarned} points earned
          </p>
          <div className="hero-links" style={{ justifyContent: 'center', marginTop: '1rem' }}>
            <Link to="/quizzes" className="btn-primary">Back to Quizzes</Link>
            <Link to="/leaderboard" className="btn-outline">Leaderboard</Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container">
      <div className="page-header" style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <h1 style={{ flex: 1 }}>{quiz.title}</h1>
        <span className={`badge ${diffClass[quiz.difficulty] ?? ''}`}>{quiz.difficulty}</span>
      </div>
      {quiz.description && <p style={{ color: 'var(--muted)', marginBottom: '1rem' }}>{quiz.description}</p>}

      {questions.map((q, i) => (
        <div key={q.id} className="card">
          <h2 style={{ fontSize: '1.05rem' }}>{i + 1}. {q.text}</h2>
          <div style={{ display: 'grid', gap: '0.5rem', marginTop: '0.75rem' }}>
            {q.options.map(o => {
              const selected = answers[q.id] === o.id;
              return (
                <button
                  key={o.id}
                  type="button"
                  onClick={() => select(q.id, o.id)}
                  className={selected ? 'btn-primary' : 'btn-outline'}
                  style={{ textAlign: 'left', width: '100%' }}
                >
                  {o.text}
                </button>
              );
            })}
          </div>
        </div>
      ))}

      <div className="card" style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <span className="stat" style={{ flex: 1 }}>
          Answered <span>{Object.keys(answers).length}/{questions.length}</span>
        </span>
        <button
          type="button"
          className="btn-primary"
          disabled={!allAnswered || submitting}
          style={{ opacity: allAnswered && !submitting ? 1 : 0.5 }}
          onClick={submit}
        >
          {submitting ? 'Submitting...' : 'Submit Quiz'}
        </button>
      </div>
    </div>
  );
}
