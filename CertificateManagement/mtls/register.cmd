openssl req -new -newkey rsa:2048 -keyout register.key -sha256 -nodes -out register.csr -config register.cnf
openssl req -in register.csr -noout -text
openssl x509 -req -days 1826 -in register.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out register.pem -extfile register.ext
openssl pkcs12 -inkey register.key -in register.pem -export -out register.pfx
openssl pkcs12 -in register.pfx -noout -info
