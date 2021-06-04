openssl req -new -newkey rsa:2048 -keyout mock-data-holder.key -sha256 -nodes -out mock-data-holder.csr -config mock-data-holder.cnf
openssl req -in mock-data-holder.csr -noout -text
openssl x509 -req -days 400 -in mock-data-holder.csr -out mock-data-holder.pem -extfile mock-data-holder.ext -signkey mock-data-holder.key
openssl pkcs12 -inkey mock-data-holder.key -in mock-data-holder.pem -export -out mock-data-holder.pfx
openssl pkcs12 -in mock-data-holder.pfx -noout -info