openssl req -new -newkey rsa:2048 -keyout mock-register.key -sha256 -nodes -out mock-register.csr -config mock-register.cnf
openssl req -in mock-register.csr -noout -text
openssl x509 -req -days 1826 -in mock-register.csr -CA ..\mtls\ca.pem -CAkey ..\mtls\ca.key -CAcreateserial -out mock-register.pem -extfile mock-register.ext
openssl pkcs12 -inkey mock-register.key -in mock-register.pem -export -out mock-register.pfx
openssl pkcs12 -in mock-register.pfx -noout -info