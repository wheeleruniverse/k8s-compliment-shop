{{/*
Expand the name of the chart.
*/}}
{{- define "k8s-compliment-shop.name" -}}
{{- default .Chart.Name .Values.global.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "k8s-compliment-shop.fullname" -}}
{{- if .Values.global.fullnameOverride }}
{{- .Values.global.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.global.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "k8s-compliment-shop.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "k8s-compliment-shop.labels" -}}
helm.sh/chart: {{ include "k8s-compliment-shop.chart" . }}
{{ include "k8s-compliment-shop.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "k8s-compliment-shop.selectorLabels" -}}
app.kubernetes.io/name: {{ include "k8s-compliment-shop.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Web Service labels
*/}}
{{- define "k8s-compliment-shop.webService.labels" -}}
{{ include "k8s-compliment-shop.labels" . }}
app: {{ .Values.webService.name }}
{{- with .Values.webService.labels }}
{{ toYaml . }}
{{- end }}
{{- end }}

{{/*
BFF Service labels
*/}}
{{- define "k8s-compliment-shop.bffService.labels" -}}
{{ include "k8s-compliment-shop.labels" . }}
app: {{ .Values.bffService.name }}
{{- with .Values.bffService.labels }}
{{ toYaml . }}
{{- end }}
{{- end }}

{{/*
Product Service labels
*/}}
{{- define "k8s-compliment-shop.productService.labels" -}}
{{ include "k8s-compliment-shop.labels" . }}
app: {{ .Values.productService.name }}
{{- with .Values.productService.labels }}
{{ toYaml . }}
{{- end }}
{{- end }}

{{/*
MySQL labels
*/}}
{{- define "k8s-compliment-shop.mysql.labels" -}}
{{ include "k8s-compliment-shop.labels" . }}
app: {{ .Values.mysql.name }}
{{- with .Values.mysql.labels }}
{{ toYaml . }}
{{- end }}
{{- end }}

{{/*
MySQL connection string
*/}}
{{- define "k8s-compliment-shop.mysql.connectionString" -}}
Server=mysql-service;Port=3306;Database={{ .Values.mysql.auth.database }};User=root;Password={{ .Values.mysql.auth.rootPassword }};
{{- end }}
