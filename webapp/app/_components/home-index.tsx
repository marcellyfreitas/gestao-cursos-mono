'use client';

import Link from 'next/link';
import Image from 'next/image';
import ImageLogo from '@/assets/images/ibca_logo_square.png';
import { useState, useEffect } from 'react';
import { useTheme } from 'next-themes';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Sheet, SheetContent, SheetTrigger, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import { Switch } from '@/components/ui/switch';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  Users,
  BookOpen,
  Mail,
  Phone,
  MapPin,
  ArrowRight,
  Sun,
  Moon,
  Menu,
  ChevronUp,
  Heart,
  GraduationCap,
} from 'lucide-react';

function Header() {
  const { theme, setTheme } = useTheme();
  const [isOpen, setIsOpen] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const navLinks = [
    { href: '#courses', label: 'Cursos' },
    { href: '#about', label: 'Sobre' },
    { href: '#leaders', label: 'Professores' },
    { href: '#contact', label: 'Contato' },
  ];

  const handleNavClick = (href: string) => {
    setIsOpen(false);
    const element = document.querySelector(href);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <Link href="/" className="flex items-center space-x-3">
          <Image
            src={ImageLogo}
            alt="Logo IBCA"
            width={40}
            height={40}
            className="h-10 w-auto"
          />
          <span className="font-bold text-xl">Escolas IBCA</span>
        </Link>

        <nav className="hidden md:flex items-center space-x-6">
          {navLinks.map((link) => (
            <button
              key={link.href}
              onClick={() => {
                const element = document.querySelector(link.href);
                if (element) {
                  element.scrollIntoView({ behavior: 'smooth' });
                }
              }}
              className="text-sm font-medium transition-colors hover:text-primary bg-transparent border-none cursor-pointer"
            >
              {link.label}
            </button>
          ))}
        </nav>

        <div className="hidden md:flex items-center space-x-4">
          {mounted && (
            <div className="flex items-center space-x-2">
              <Sun className="h-4 w-4" />
              <Switch
                checked={theme === 'dark'}
                onCheckedChange={(checked) => setTheme(checked ? 'dark' : 'light')}
              />
              <Moon className="h-4 w-4" />
            </div>
          )}
          <Button asChild variant="outline">
            <Link href="/authentication/login">Entrar</Link>
          </Button>
        </div>

        <Sheet open={isOpen} onOpenChange={setIsOpen}>
          <SheetTrigger asChild className="md:hidden">
            <Button variant="ghost" size="icon">
              <Menu className="h-5 w-5" />
            </Button>
          </SheetTrigger>
          <SheetContent side="right" className="w-[300px] sm:w-[400px] px-6 py-6">
            <SheetHeader className="mb-4">
              <SheetTitle>Menu de Navegação</SheetTitle>
            </SheetHeader>
            <nav className="flex flex-col space-y-6 mt-8">
              {navLinks.map((link) => (
                <button
                  key={link.href}
                  onClick={() => handleNavClick(link.href)}
                  className="text-lg font-medium transition-colors hover:text-primary text-left bg-transparent border-none cursor-pointer"
                >
                  {link.label}
                </button>
              ))}
              <div className="flex flex-col space-y-4 pt-6 border-t">
                {mounted && (
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium">Tema</span>
                    <div className="flex items-center space-x-2">
                      <Sun className="h-4 w-4" />
                      <Switch
                        checked={theme === 'dark'}
                        onCheckedChange={(checked) => setTheme(checked ? 'dark' : 'light')}
                      />
                      <Moon className="h-4 w-4" />
                    </div>
                  </div>
                )}
                <Button asChild variant="outline" className="w-full">
                  <Link href="/authentication/login" onClick={() => setIsOpen(false)}>
                    Entrar
                  </Link>
                </Button>
              </div>
            </nav>
          </SheetContent>
        </Sheet>
      </div>
    </header>
  );
}

function HeroSection() {
  const handleScrollToCourses = () => {
    const element = document.querySelector('#courses');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  };

  return (
    <section className="relative overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-background to-secondary/10" />
      <div className="container mx-auto px-4 py-24 md:py-32 lg:py-40 relative">
        <div className="text-center max-w-4xl mx-auto">
          <Heart className="h-12 w-12 text-primary mx-auto mb-6" />
          <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight mb-6">
            Cresça no conhecimento e na fé
          </h1>
          <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto leading-relaxed">
            Cursos internos para fortalecer sua caminhada espiritual e edificar
            vidas através do ensino bíblico.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" onClick={handleScrollToCourses}>
              Ver Cursos
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
            <Button size="lg" variant="outline" asChild>
              <Link href="/authentication/login">Entrar</Link>
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
}

const mockCourses = [
  {
    id: 1,
    name: 'Fundamentos da Fé',
    teacher: 'Pr. João Silva',
    category: 'Básico',
    description: 'Aprenda os princípios fundamentais da fé cristã.',
  },
  {
    id: 2,
    name: 'Liderança Cristã',
    teacher: 'Dra. Maria Santos',
    category: 'Avançado',
    description: 'Desenvolva habilidades de liderança baseadas em princípios bíblicos.',
  },
  {
    id: 3,
    name: 'Estudo Bíblico',
    teacher: 'Pr. Paulo Oliveira',
    category: 'Intermediário',
    description: 'Aprofunde seu conhecimento nas Escrituras.',
  },
];

function FeaturedCoursesSection() {
  return (
    <section id="courses" className="container mx-auto px-4 py-24 scroll-mt-16">
      <div className="text-center mb-16">
        <Badge variant="secondary" className="mb-4">
          Cursos em Destaque
        </Badge>
        <h2 className="text-3xl md:text-4xl font-bold mb-4">
          Transforme sua vida através do estudo
        </h2>
        <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
          Conheça alguns dos nossos cursos mais impactantes.
        </p>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {mockCourses.map((course) => (
          <Card
            key={course.id}
            className="transition-all duration-300 hover:-translate-y-1 hover:shadow-lg"
          >
            <CardHeader>
              <div className="flex items-center justify-between mb-2">
                <GraduationCap className="h-8 w-8 text-primary" />
                <Badge variant="outline">{course.category}</Badge>
              </div>
              <CardTitle className="text-xl">{course.name}</CardTitle>
              <CardDescription className="text-sm text-muted-foreground">
                {course.teacher}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-4">
                {course.description}
              </p>
              <Button className="w-full" variant="outline" asChild>
                <Link href="/authentication/login">Acessar</Link>
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}

function AboutSection() {
  return (
    <section id="about" className="border-t bg-muted/30">
      <div className="container mx-auto px-4 py-24 scroll-mt-16">
        <div className="max-w-3xl mx-auto text-center">
          <BookOpen className="h-12 w-12 text-primary mx-auto mb-6" />
          <h2 className="text-3xl md:text-4xl font-bold mb-6">
            Nossa Missão
          </h2>
          <p className="text-lg text-muted-foreground leading-relaxed mb-8">
            Capacitar e edificar vidas por meio do ensino cristão, proporcionando
            uma formação sólida baseada na Palavra de Deus. Nossos cursos são
            desenhados para fortalecer sua fé e preparar você para o ministério.
          </p>
          <div className="flex items-center justify-center space-x-8 text-sm text-muted-foreground">
            <div className="flex items-center space-x-2">
              <Users className="h-5 w-5" />
              <span>+500 alunos</span>
            </div>
            <div className="flex items-center space-x-2">
              <BookOpen className="h-5 w-5" />
              <span>15+ cursos</span>
            </div>
            <div className="flex items-center space-x-2">
              <Heart className="h-5 w-5" />
              <span>10+ professores</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

const mockLeaders = [
  {
    name: 'Pr. João Silva',
    role: 'Teologia Sistemática',
    initials: 'JS',
  },
  {
    name: 'Dra. Maria Santos',
    role: 'Liderança Cristã',
    initials: 'MS',
  },
  {
    name: 'Pr. Paulo Oliveira',
    role: 'Estudo Bíblico',
    initials: 'PO',
  },
  {
    name: 'Profa. Ana Costa',
    role: 'Ministério de Mulheres',
    initials: 'AC',
  },
];

function LeadersSection() {
  return (
    <section id="leaders" className="container mx-auto px-4 py-24 scroll-mt-16">
      <div className="text-center mb-16">
        <h2 className="text-3xl md:text-4xl font-bold mb-4">
          Nossos Professores
        </h2>
        <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
          Conheça os líderes espirituais que ministram nossos cursos.
        </p>
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 max-w-5xl mx-auto">
        {mockLeaders.map((leader) => (
          <div
            key={leader.name}
            className="text-center transition-all duration-300 hover:-translate-y-1"
          >
            <Avatar className="h-24 w-24 mx-auto mb-4">
              <AvatarFallback className="text-lg bg-primary/10 text-primary">
                {leader.initials}
              </AvatarFallback>
            </Avatar>
            <h3 className="text-lg font-semibold mb-1">{leader.name}</h3>
            <p className="text-sm text-muted-foreground">{leader.role}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function ContactSection() {
  return (
    <section id="contact" className="border-t bg-muted/30">
      <div className="container mx-auto px-4 py-24 scroll-mt-16">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-bold mb-4">Contato</h2>
          <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
            Entre em contato conosco para mais informações.
          </p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 max-w-4xl mx-auto">
          <Card className="text-center">
            <CardHeader>
              <Mail className="h-8 w-8 text-primary mx-auto mb-2" />
              <CardTitle className="text-lg">Email</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">contato@igreja.com</p>
            </CardContent>
          </Card>
          <Card className="text-center">
            <CardHeader>
              <Phone className="h-8 w-8 text-primary mx-auto mb-2" />
              <CardTitle className="text-lg">Telefone</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">(11) 99999-9999</p>
            </CardContent>
          </Card>
          <Card className="text-center">
            <CardHeader>
              <MapPin className="h-8 w-8 text-primary mx-auto mb-2" />
              <CardTitle className="text-lg">Endereço</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">
                Rua da Fé, 123 - Centro, SP
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </section>
  );
}

function FinalCTASection() {
  return (
    <section className="container mx-auto px-4 py-24 text-center">
      <h2 className="text-3xl md:text-4xl font-bold mb-4">
        Pronto para começar sua jornada?
      </h2>
      <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
        Junte-se a centenas de alunos que estão crescendo espiritualmente.
      </p>
      <Button size="lg" asChild>
        <Link href="/authentication/login">
          Acessar Plataforma
          <ArrowRight className="ml-2 h-4 w-4" />
        </Link>
      </Button>
    </section>
  );
}

function Footer() {
  return (
    <footer className="border-t py-8">
      <div className="container mx-auto px-4">
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center space-x-3">
            <Image
              src={ImageLogo}
              alt="Logo IBCA"
              width={32}
              height={32}
              className="h-8 w-auto"
            />
            <span className="font-semibold">Escolas IBCA</span>
          </div>
          <p className="text-sm text-muted-foreground text-center">
            © 2026 Escolas IBCA. Todos os direitos reservados.
          </p>
          <div className="flex items-center space-x-4">
            <Link
              href="/terms"
              className="text-sm text-muted-foreground hover:text-primary transition-colors"
            >
              Termos
            </Link>
            <Link
              href="/privacy"
              className="text-sm text-muted-foreground hover:text-primary transition-colors"
            >
              Privacidade
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}

function BackToTopButton() {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const toggleVisibility = () => {
      if (window.scrollY > 300) {
        setIsVisible(true);
      } else {
        setIsVisible(false);
      }
    };

    window.addEventListener('scroll', toggleVisibility);
    return () => window.removeEventListener('scroll', toggleVisibility);
  }, []);

  const scrollToTop = () => {
    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  };

  if (!isVisible) return null;

  return (
    <Button
      onClick={scrollToTop}
      size="icon"
      className="fixed bottom-8 right-8 z-50 h-12 w-12 rounded-full shadow-lg transition-all duration-300 hover:scale-110 flex items-center justify-center"
      aria-label="Voltar ao topo"
    >
      <ChevronUp className="h-5 w-5" />
    </Button>
  );
}

export default function HomeIndex() {
  return (
    <main className="flex min-h-screen flex-col scroll-smooth">
      <Header />
      <HeroSection />
      <FeaturedCoursesSection />
      <AboutSection />
      <LeadersSection />
      <ContactSection />
      <FinalCTASection />
      <Footer />
      <BackToTopButton />
    </main>
  );
}
