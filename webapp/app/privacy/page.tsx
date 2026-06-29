import Link from 'next/link';
import { ArrowLeft } from 'lucide-react';

export default function PrivacyPage() {
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

        <h1 className="text-3xl md:text-4xl font-bold mb-8">Política de Privacidade</h1>

        <div className="space-y-8 text-muted-foreground leading-relaxed">
          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">1. Informações Gerais</h2>
            <p>
              A Escolas IBCA respeita sua privacidade e está comprometida com a proteção
              dos dados pessoais de nossos usuários. Esta política descreve como coletamos,
              usamos e protegemos suas informações.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">2. Dados Coletados</h2>
            <p>
              Coletamos informações fornecidas voluntariamente durante o cadastro, como
              nome, email, telefone e dados acadêmicos. Também coletamos dados de uso
              da plataforma para melhorar nossos serviços.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">3. Uso das Informações</h2>
            <p>
              Utilizamos seus dados para gerenciar sua conta, processar matrículas,
              enviar comunicações relacionadas aos cursos e melhorar a experiência
              na plataforma. Seus dados não serão vendidos ou compartilhados com
              terceiros sem seu consentimento.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">4. Armazenamento e Segurança</h2>
            <p>
              Adotamos medidas técnicas e organizacionais para proteger seus dados
              contra acesso não autorizado, perda ou alteração. Os dados são armazenados
              em servidores seguros com criptografia adequada.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">5. Compartilhamento de Dados</h2>
            <p>
              Seus dados pessoais são compartilhados apenas com a equipe administrativa
              da igreja para fins de gestão acadêmica. Não compartilhamos dados com
              terceiros, exceto quando exigido por lei.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">6. Cookies</h2>
            <p>
              Utilizamos cookies para manter sua sessão ativa e melhorar a experiência
              de navegação. Você pode configurar seu navegador para recusar cookies,
              mas isso pode afetar a funcionalidade da plataforma.
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">7. Seus Direitos</h2>
            <p>
              Você tem direito a acessar, corrigir ou solicitar a exclusão de seus
              dados pessoais. Para exercer estes direitos, entre em contato conosco
              através do email: contato@igreja.com
            </p>
          </section>

          <section>
            <h2 className="text-xl font-semibold text-foreground mb-3">8. Alterações na Política</h2>
            <p>
              Esta política pode ser atualizada periodicamente. Recomendamos que você
              revise esta página regularmente para estar ciente de quaisquer alterações.
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
