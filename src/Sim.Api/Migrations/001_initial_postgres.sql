CREATE TABLE sims (id uuid PRIMARY KEY, phone_number text NOT NULL, carrier text NOT NULL, status text NOT NULL, activation_date date NOT NULL, expiry_date date NOT NULL);
CREATE TABLE customers (id uuid PRIMARY KEY, full_name text NOT NULL, identity_number text NOT NULL, phone_numbers text[] NOT NULL);
CREATE TABLE collaborators (id uuid PRIMARY KEY, full_name text NOT NULL, phone_number text NOT NULL, email text NULL);
CREATE TABLE orders (id uuid PRIMARY KEY, customer_id uuid NOT NULL REFERENCES customers(id), sim_id uuid NOT NULL REFERENCES sims(id), collaborator_id uuid NULL REFERENCES collaborators(id), status text NOT NULL, revenue numeric(18,2) NOT NULL, ordered_at date NOT NULL);
CREATE INDEX ix_sims_expiry_date ON sims(expiry_date);
CREATE INDEX ix_orders_ordered_at ON orders(ordered_at);
