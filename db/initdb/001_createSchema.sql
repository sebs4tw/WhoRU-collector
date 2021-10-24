CREATE TABLE loginevents (
  time TIMESTAMP NOT NULL,
  connectionType varchar(100) NOT NULL,
  level varchar(25) NOT NULL,
  extraprops JSONB
);

CREATE INDEX ON loginevents USING BTREE (((extraprops->'Email')::text), time DESC);
SELECT create_hypertable('loginevents', 'time', chunk_time_interval => INTERVAL '1 day');