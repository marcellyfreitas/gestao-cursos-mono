import Link from 'next/link';
import { ArrowLeft } from 'lucide-react';

export default function TermsPage() {
  return (
    <main className="min-h-screen bg-background">
      <div className="container mx-auto max-w-3xl px-4 py-16">
        <Link
          href="/"
          className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary transition-colors mb-8"
        >
          <ArrowLeft className="h-4 w-4" />
          Voltar para home
        </Link>

        <h1 className="text-3xl md:text-4xl font-bold mb-8">Termos de Uso</h1>

        <div className="space-y-8 text-muted-foreground leading-relaxed">
          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">1. Aceitação dos Termos</h2>
            <p>
              Ao acessar e utilizar a plataforma Escolas IBCA, você concorda com estes Termos de Uso.
              Caso não concorde com qualquer parte destes termos, pedimos que não utilize nossos serviços.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">2. Uso da Plataforma</h2>
            <p>
              A plataforma Escolas IBCA destina-se exclusivamente para fins de gestão de cursos internos
              da igreja. O usuário compromete-se a utilizar o sistema de forma ética e responsável,
              respeitando as normas da instituição.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">3. Cadastro e Conta</h2>
            <p>
              Para acessar determinadas funcionalidades, o usuário deve realizar seu cadastro
              fornecendo informações verídicas. É de responsabilidade do usuário manter a
              confidencialidade de sua senha e dados de acesso.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">4. Conduta do Usuário</h2>
            <p>
              O usuário concorda em não utilizar a plataforma para fins ilícitos, difamatórios
              ou que violem direitos de terceiros. O usuário respeitará o ambiente cristão e
              os valores da instituição.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">5. Propriedade Intelectual</h2>
            <p>
              Todo o conteúdo disponibilizado na plataforma, incluindo materiais de curso,
              textos, imagens e logos, são propriedade da IBCA e protegidos por direitos autorais.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">6. Limitação de Responsabilidade</h2>
            <p>
              A Escolas IBCA não se responsabiliza por interrupções temporárias no serviço
              decorrentes de manutenção ou falhas técnicas, empenhando-se para manter a
              plataforma sempre disponível.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">7. Alterações nos Termos</h2>
            <p>
              Reservamos o direito de modificar estes termos a qualquer momento. As alterações
              entrarão em vigor imediatamente após sua publicação na plataforma.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">8. Contato</h2>
            <p>
              Para dúvidas sobre estes Termos de Uso, entre em contato através do email:
              contato@igreja.com
            </p>
          </section>
        </div>

        <div className="mt-12 pt-8 border-t">
          <p className="text-sm text-muted-foreground">
            Última atualização: 1 de maio de 2026
          </p>
        </div>
      </div>
    </main>
  );
}
