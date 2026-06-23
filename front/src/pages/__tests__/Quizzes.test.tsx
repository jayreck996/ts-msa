import { render, screen, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import Quizzes from '../Quizzes';
import { api } from '../../api';

vi.mock('../../api', () => ({
  api: {
    getQuizzes: vi.fn(),
  },
}));

describe('Quizzes', () => {
  it('shows loading initially', () => {
    vi.mocked(api.getQuizzes).mockReturnValue(new Promise(() => {}));
    render(<Quizzes />);
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('renders quiz list', async () => {
    vi.mocked(api.getQuizzes).mockResolvedValue([
      { id: 1, title: 'C# Basics', description: '', difficulty: 'Easy', categoryId: 1, category: { id: 1, name: 'Tech' } },
    ]);
    render(<Quizzes />);
    await waitFor(() => expect(screen.getByText(/C# Basics/i)).toBeInTheDocument());
    expect(screen.getByText(/Easy/i)).toBeInTheDocument();
  });

  it('shows empty message when no quizzes', async () => {
    vi.mocked(api.getQuizzes).mockResolvedValue([]);
    render(<Quizzes />);
    await waitFor(() => expect(screen.getByText(/no quizzes yet/i)).toBeInTheDocument());
  });

  it('shows error when fetch fails', async () => {
    vi.mocked(api.getQuizzes).mockRejectedValue(new Error('Network error'));
    render(<Quizzes />);
    await waitFor(() => expect(screen.getByText(/failed to load/i)).toBeInTheDocument());
  });
});
