FROM nginx:alpine

# Instala o Certbot e o plugin nginx
RUN apk add --no-cache certbot certbot-nginx

# Cria diretório padrão do Let's Encrypt
RUN mkdir -p /etc/letsencrypt

EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]
