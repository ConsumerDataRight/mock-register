openssl req -new -newkey rsa:2048 -keyout tls-template.key -sha256 -nodes -out tls-template.csr -config tls-template.cnf
openssl req -in tls-template.csr -noout -text
openssl x509 -req -days 400 -in tls-template.csr -CA ..\mtls\ca.pem -CAkey ..\mtls\ca.key -CAcreateserial -out tls-template.pem -extfile tls-template.ext
openssl pkcs12 -inkey tls-template.key -in tls-template.pem -export -out tls-template.pfx
openssl pkcs12 -in tls-template.pfx -noout -info
