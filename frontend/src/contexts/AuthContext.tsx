import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import axios from 'axios';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  city?: string;
  address?: string;
  profilePictureUrl?: string;
  isAdmin?: boolean;
  isShelter: boolean;
  isShelterVerified?: boolean;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (data: any) => Promise<void>;
  logout: () => void;
  updateUser: (userData: Partial<User>) => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    const storedUser = localStorage.getItem('user');
    
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
      axios.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`;
    }
    setIsLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    const response = await axios.post('http://localhost:5147/api/auth/login', {
      email,
      password,
    });
    
    const data = response.data;
    const user = {
      id: data.userId,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      phoneNumber: data.phoneNumber,
      city: data.city,
      address: data.address,
      profilePictureUrl: data.profilePictureUrl,
      isAdmin: data.isAdmin,
      isShelter: data.isShelter,
      isShelterVerified: data.isShelterVerified,
    };
    
    setToken(data.token);
    setUser(user);
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(user));
    axios.defaults.headers.common['Authorization'] = `Bearer ${data.token}`;
  };

  const register = async (data: any) => {
    const response = await axios.post('http://localhost:5147/api/auth/register', data);
    
    const responseData = response.data;
    const user = {
      id: responseData.userId,
      email: responseData.email,
      firstName: responseData.firstName,
      lastName: responseData.lastName,
      phoneNumber: responseData.phoneNumber,
      city: responseData.city,
      address: responseData.address,
      profilePictureUrl: responseData.profilePictureUrl,
      isAdmin: responseData.isAdmin,
      isShelter: responseData.isShelter,
      isShelterVerified: responseData.isShelterVerified,
    };
    
    setToken(responseData.token);
    setUser(user);
    localStorage.setItem('token', responseData.token);
    localStorage.setItem('user', JSON.stringify(user));
    axios.defaults.headers.common['Authorization'] = `Bearer ${responseData.token}`;
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    delete axios.defaults.headers.common['Authorization'];
  };

  const updateUser = (userData: Partial<User>) => {
    if (user) {
      const updatedUser = { ...user, ...userData };
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
    }
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout, updateUser, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

