FROM alpine:latest

RUN apk add --no-cache doxygen graphviz ttf-freefont

WORKDIR /app

COPY Doxyfile /app/
COPY doxygen-awesome.css /app/

CMD ["doxygen", "Doxyfile"]
