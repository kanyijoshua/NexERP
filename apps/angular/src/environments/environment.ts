import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'ABPmicroservice',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:7600/',
    redirectUri: baseUrl,
    clientId: 'ABPmicroservice_Angular',
    responseType: 'code',
    scope: 'ABPmicroserviceIdentityService ABPmicroserviceAdministration ABPmicroserviceSaaS',
    requireHttps: false,
  },
  apis: {
    default: {
      url: 'https://localhost:7500',
      rootNamespace: 'ABPmicroservice',
    },
  },
} as Environment;
