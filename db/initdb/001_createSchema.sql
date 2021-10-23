CREATE TABLE loginevents (
  time TIMESTAMP NOT NULL,
  account text,
  connectionType varchar(100) NOT NULL,
  level varchar(25) NOT NULL,
  extraProps JSONB
);

CREATE INDEX ON loginevents (account, time DESC);
SELECT create_hypertable('loginevents', 'time', chunk_time_interval => INTERVAL '1 day');
ALTER TABLE loginevents SET (
  timescaledb.compress,
  timescaledb.compress_segmentby = 'account'
);
SELECT add_compression_policy('loginevents', INTERVAL '30 days');