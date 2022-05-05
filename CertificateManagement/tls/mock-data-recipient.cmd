openssl req -new -newkey rsa:2048 -keyout mock-data-recipient.key -sha256 -nodes -out mock-data-recipient.csr -config mock-data-recipient.cnf
openssl req -in mock-data-recipient.csr -noout -text
openssl x509 -req -days 400 -in mock-data-recipient.csr -CA ..\mtls\ca.pem -CAkey ..\mtls\ca.key -CAcreateserial -out mock-data-recipient.pem -extfile mock-data-recipient.ext
openssl pkcs12 -inkey mock-data-recipient.key -in mock-data-recipient.pem -export -out mock-data-recipient.pfx
openssl pkcs12 -in mock-data-recipient.pfx -noout -info