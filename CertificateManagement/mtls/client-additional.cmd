openssl req -new -newkey rsa:2048 -keyout client-additional.key -sha256 -nodes -out client-additional.csr -config client-additional.cnf
openssl req -in client-additional.csr -noout -text
openssl x509 -req -days 1826 -in client-additional.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out client-additional.pem -extfile client.ext
openssl pkcs12 -inkey client-additional.key -in client-additional.pem -export -out client-additional.pfx
openssl pkcs12 -in client-additional.pfx -noout -info
