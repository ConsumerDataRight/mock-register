openssl req -new -newkey rsa:2048 -keyout mock-register.key -sha256 -nodes -out mock-register.csr -config mock-register.cnf
openssl req -in mock-register.csr -noout -text
openssl x509 -req -days 400 -in mock-register.csr -out mock-register.pem -extfile mock-register.ext -signkey mock-register.key
openssl pkcs12 -inkey mock-register.key -in mock-register.pem -export -out mock-register.pfx
openssl pkcs12 -in mock-register.pfx -noout -info