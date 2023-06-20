openssl req -new -newkey rsa:2048 -keyout server-energy.key -sha256 -nodes -out server-energy.csr -config server-energy.cnf
openssl req -in server-energy.csr -noout -text
openssl x509 -req -days 1826 -in server-energy.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out server-energy.pem -extfile server-energy.ext
openssl pkcs12 -inkey server-energy.key -in server-energy.pem -export -out server-energy.pfx
openssl pkcs12 -in server-energy.pfx -noout -info
