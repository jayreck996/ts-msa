const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}/api/${path}`, options);
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json();
}

export const api = {
  // Users
  getUsers: () => request<User[]>('users'),
  getUser: (id: number) => request<User>(`users/${id}`),
  createUser: (body: Partial<User>) => request<User>('users', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }),

  // Categories
  getCategories: () => request<Category[]>('categories'),

  // Quizzes
  getQuizzes: (params?: { difficulty?: string; categoryId?: number }) => {
    const q = new URLSearchParams(params as Record<string, string>).toString();
    return request<Quiz[]>(`quizzes${q ? `?${q}` : ''}`);
  },
  getQuiz: (id: number) => request<Quiz>(`quizzes/${id}`),

  // Questions & Options
  getQuestions: (quizId: number) => request<Question[]>(`questions?quizId=${quizId}`),
  getOptions: (questionId: number) => request<Option[]>(`options?questionId=${questionId}`),

  // Attempts
  submitAttempt: (body: Partial<QuizAttempt>) => request<QuizAttempt>('attempts', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }),

  // Badges
  getBadges: () => request<Badge[]>('badges'),
  getUserBadges: (userId: number) => request<UserBadge[]>(`badges/user/${userId}`),

  // Leaderboard
  getLeaderboard: (top = 10) => request<LeaderboardEntry[]>(`leaderboard?top=${top}`),
};

export interface User { id: number; username: string; email: string; totalPoints: number; level: number; currentStreak: number; longestStreak: number; }
export interface Category { id: number; name: string; }
export interface Quiz { id: number; title: string; description: string; difficulty: string; categoryId: number; category: Category; }
export interface Question { id: number; quizId: number; text: string; points: number; correctOptionId: number | null; options: Option[]; }
export interface Option { id: number; questionId: number; text: string; }
export interface QuizAttempt { id: number; userId: number; quizId: number; score: number; pointsEarned: number; completedAt: string; }
export interface Badge { id: number; name: string; description: string; requirement: string; }
export interface UserBadge { id: number; userId: number; badgeId: number; earnedAt: string; badge: Badge; }
export interface LeaderboardEntry { id: number; username: string; totalPoints: number; level: number; currentStreak: number; }
