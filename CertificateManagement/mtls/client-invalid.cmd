openssl req -new -newkey rsa:2048 -keyout client-invalid.key -sha256 -nodes -out client-invalid.csr -config client-invalid.cnf
openssl req -in client-invalid.csr -noout -text
openssl x509 -req -days 1 -in client-invalid.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out client-invalid.pem -extfile client.ext
openssl pkcs12 -inkey client-invalid.key -in client-invalid.pem -export -out client-invalid.pfx
openssl pkcs12 -in client-invalid.pfx -noout -info
