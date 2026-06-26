import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import Home from '../Home';

describe('Home', () => {
  it('renders the heading', () => {
    render(<MemoryRouter><Home /></MemoryRouter>);
    expect(screen.getByRole('heading', { name: /QuizQuest/i })).toBeInTheDocument();
  });

  it('renders the tagline', () => {
    render(<MemoryRouter><Home /></MemoryRouter>);
    expect(screen.getByText(/earn points/i)).toBeInTheDocument();
  });
});
