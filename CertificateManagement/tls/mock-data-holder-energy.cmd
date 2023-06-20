openssl req -new -newkey rsa:2048 -keyout mock-data-holder-energy.key -sha256 -nodes -out mock-data-holder-energy.csr -config mock-data-holder-energy.cnf
openssl req -in mock-data-holder-energy.csr -noout -text
openssl x509 -req -days 1826 -in mock-data-holder-energy.csr -CA ..\mtls\ca.pem -CAkey ..\mtls\ca.key -CAcreateserial -out mock-data-holder-energy.pem -extfile mock-data-holder-energy.ext
openssl pkcs12 -inkey mock-data-holder-energy.key -in mock-data-holder-energy.pem -export -out mock-data-holder-energy.pfx
openssl pkcs12 -in mock-data-holder-energy.pfx -noout -info
