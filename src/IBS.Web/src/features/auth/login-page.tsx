import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { api, getErrorMessage } from "@/lib/api";
import { useAuthStore } from "@/stores/auth";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

type LoginFormData = {
  email: string;
  password: string;
};

/**
 * Login page component
 */
export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setUser = useAuthStore((state) => state.setUser);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loginSchema = z.object({
    email: z.string().email(t('common.errors.invalidEmail')),
    password: z.string().min(1, t('common.errors.passwordRequired')),
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await api.post("/auth/login", data);
      // Tokens are delivered as httpOnly cookies — no localStorage writes needed
      const { userId, tenantId, email, fullName, roles } = response.data;

      // Update auth state
      const [firstName, ...rest] = (fullName as string).split(' ');
      setUser({
        id: userId,
        tenantId,
        email,
        firstName: firstName ?? '',
        lastName: rest.join(' '),
        roles,
      });

      // Navigate to dashboard
      navigate("/");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <div className="flex items-center justify-center gap-2 mb-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary">
              <span className="text-2xl font-bold text-primary-foreground">
                IBS
              </span>
            </div>
          </div>
          <CardTitle className="text-2xl text-center">{t('auth.title')}</CardTitle>
          <CardDescription className="text-center">
            {t('auth.subtitle')}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {error && (
              <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
                {error}
              </div>
            )}

            <div className="space-y-2">
              <Label htmlFor="email" required>
                {t('auth.email')}
              </Label>
              <Input
                id="email"
                type="email"
                placeholder="you@example.com"
                error={errors.email?.message}
                {...register("email")}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="password" required>
                {t('auth.password')}
              </Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                error={errors.password?.message}
                {...register("password")}
              />
            </div>

            <Button type="submit" className="w-full" isLoading={isLoading}>
              {t('auth.signIn')}
            </Button>

            <div className="text-center text-sm text-muted-foreground">
              <a href="/forgot-password" className="hover:text-primary">
                {t('auth.forgotPassword')}
              </a>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
