import { Skeleton } from '@/components/ui/skeleton';

interface FormLoadingProviderProps {
  loading: boolean;
  children: React.ReactNode;
}

export const FormLoadingProvider = ({ loading, children }: FormLoadingProviderProps) => {
  if (loading) {
    return (
      <div className="mt-4 grid gap-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
        <div className="grid grid-cols-2 gap-4">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
        <Skeleton className="h-10 w-full" />
        <div className="grid grid-cols-2 gap-4">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </div>
    );
  }
  return <>{children}</>;
};
